// Backup & Restore Page
import { escapeHtml, formatBytes, formatDate, uploadWithProgress, progressBar, setProgress } from '../staticuploads/common.js';

document.addEventListener('DOMContentLoaded', () => {
  const app = document.getElementById('app');

  app.innerHTML = `
    <h2>Backup &amp; Restore</h2>
    <div id="message"></div>

    <h3>Current Site</h3>
    <div id="status"><p class="text-muted">Loading&hellip;</p></div>

    <h3>Download a Backup</h3>
    <p>One .zip containing the database and every uploaded static file. It includes user accounts and password hashes, so keep it somewhere safe.</p>
    <p><a href="/api/backup/download" id="downloadLink" class="btn btn-primary">Download Backup</a></p>

    <h3>Restore</h3>
    <p>Replaces all content, layouts, templates and static files with the contents of a backup. This site's user accounts are kept.
       A snapshot of the current site is saved first so the restore can be undone.</p>
    <form id="restoreForm" class="form-inline">
      <div class="form-group">
        <input type="file" id="file" accept=".zip" class="form-control" required />
      </div>
      <button type="submit" id="checkButton" class="btn btn-default">Upload and Check</button>
    </form>
    ${progressBar()}
    <div id="preview"></div>

    <h3>Snapshots</h3>
    <p>Saved automatically before each restore.</p>
    <div id="snapshots"></div>
  `;

  const messageDiv = document.getElementById('message');
  const statusDiv = document.getElementById('status');
  const previewDiv = document.getElementById('preview');
  const snapshotsDiv = document.getElementById('snapshots');
  const fileInput = document.getElementById('file');
  const checkButton = document.getElementById('checkButton');

  function showMessage(kind, html) {
    messageDiv.innerHTML = html ? `<div class="alert alert-${kind}">${html}</div>` : '';
    if (html) messageDiv.scrollIntoView({ behavior: 'smooth' });
  }

  function countsTable(counts) {
    const rows = [
      ['Content types', counts.contentTypes],
      ['Contents', counts.contents],
      ['Layouts', counts.layouts],
      ['Templates', counts.templates],
      ['Static uploads', counts.staticUploads],
      ['Static files', `${counts.staticFiles} (${formatBytes(counts.staticBytes)})`],
      ['User accounts', counts.users]
    ];
    return `
      <table class="table table-condensed" style="width:auto;">
        <tbody>${rows.map(([label, value]) => `<tr><th>${label}</th><td>${escapeHtml(value)}</td></tr>`).join('')}</tbody>
      </table>
    `;
  }

  function list(kind, title, items) {
    if (!items || items.length === 0) return '';
    return `
      <div class="alert alert-${kind}">
        <strong>${title}</strong>
        <ul>${items.map(item => `<li>${escapeHtml(item)}</li>`).join('')}</ul>
      </div>
    `;
  }

  async function loadStatus() {
    try {
      const response = await fetch('/api/backup/status');
      if (!response.ok) throw new Error('Failed to load backup status');
      const status = await response.json();

      statusDiv.innerHTML = `
        ${countsTable(status.counts)}
        <p class="text-muted">Schema: ${escapeHtml(status.migrations[status.migrations.length - 1] || 'none')}</p>
        ${list('warning', 'Warnings', status.warnings)}
      `;
      renderSnapshots(status.snapshots);
    } catch (error) {
      statusDiv.innerHTML = `<div class="alert alert-danger">Error: ${escapeHtml(error.message)}</div>`;
    }
  }

  function renderSnapshots(snapshots) {
    if (snapshots.length === 0) {
      snapshotsDiv.innerHTML = '<p class="text-muted">No snapshots yet.</p>';
      return;
    }

    snapshotsDiv.innerHTML = `
      <table class="table">
        <thead><tr><th>Name</th><th>Saved</th><th>Size</th><th>Actions</th></tr></thead>
        <tbody>
          ${snapshots.map(s => `
            <tr>
              <td>${escapeHtml(s.name)}</td>
              <td>${formatDate(s.createdAtUtc)}</td>
              <td>${formatBytes(s.sizeBytes)}</td>
              <td>
                <a href="/api/backup/snapshots/${encodeURIComponent(s.name)}">Download</a> |
                <a href="#" class="stage-snapshot" data-name="${escapeHtml(s.name)}">Check and Restore</a>
              </td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `;

    snapshotsDiv.querySelectorAll('.stage-snapshot').forEach(link => {
      link.addEventListener('click', async (e) => {
        e.preventDefault();
        showMessage(null);
        previewDiv.innerHTML = '<p class="text-muted">Checking snapshot&hellip;</p>';
        try {
          const response = await fetch(`/api/backup/snapshots/${encodeURIComponent(link.dataset.name)}/restore`, { method: 'POST' });
          const body = await response.json().catch(() => null);
          if (!response.ok) throw new Error(body?.error || `Check failed (HTTP ${response.status}).`);
          renderPreview(body);
        } catch (error) {
          previewDiv.innerHTML = `<div class="alert alert-danger">Error: ${escapeHtml(error.message)}</div>`;
        }
      });
    });
  }

  function renderPreview(preview) {
    previewDiv.innerHTML = `
      <div class="panel panel-${preview.canRestore ? 'info' : 'danger'}" style="margin-top: 15px;">
        <div class="panel-heading"><strong>${escapeHtml(preview.fileName)}</strong></div>
        <div class="panel-body">
          <p>
            Backed up ${formatDate(preview.createdAtUtc)}
            from <code>${escapeHtml(preview.sourceMachine)}</code> (${escapeHtml(preview.sourceEnvironment)}),
            DSCMS ${escapeHtml(preview.appVersion)}, backup format ${preview.formatVersion}.
          </p>
          ${countsTable(preview.counts)}
          ${list('danger', 'This backup cannot be restored', preview.errors)}
          ${list('info', 'Database upgrades that will be applied', preview.pendingMigrations)}
          ${list('warning', 'Warnings', preview.warnings)}
          ${preview.canRestore ? `
            <div class="checkbox">
              <label><input type="checkbox" id="confirmRestore" />
                I understand this replaces all content and static files on this site.</label>
            </div>
            <button type="button" id="commitButton" class="btn btn-danger" disabled>Restore Now</button>
          ` : ''}
          <button type="button" id="cancelButton" class="btn btn-default">${preview.canRestore ? 'Cancel' : 'Close'}</button>
        </div>
      </div>
    `;

    document.getElementById('cancelButton').addEventListener('click', async () => {
      previewDiv.innerHTML = '';
      if (preview.token) {
        await fetch(`/api/backup/restore/${preview.token}`, { method: 'DELETE' });
      }
    });

    if (!preview.canRestore) return;

    const confirmBox = document.getElementById('confirmRestore');
    const commitButton = document.getElementById('commitButton');
    confirmBox.addEventListener('change', () => { commitButton.disabled = !confirmBox.checked; });

    commitButton.addEventListener('click', async () => {
      commitButton.disabled = true;
      commitButton.textContent = 'Restoring...';
      try {
        const response = await fetch(`/api/backup/restore/${preview.token}/commit`, { method: 'POST' });
        const body = await response.json().catch(() => null);
        if (!response.ok) throw new Error(body?.error || `Restore failed (HTTP ${response.status}).`);

        previewDiv.innerHTML = '';
        showMessage('success', `Restore complete. The previous site was saved as snapshot <code>${escapeHtml(body.snapshotName)}</code>.`);
        await loadStatus();
      } catch (error) {
        commitButton.textContent = 'Restore Now';
        commitButton.disabled = false;
        showMessage('danger', `Error: ${escapeHtml(error.message)}`);
      }
    });
  }

  document.getElementById('restoreForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    showMessage(null);
    previewDiv.innerHTML = '';

    const formData = new FormData();
    formData.append('file', fileInput.files[0]);

    checkButton.disabled = true;
    try {
      const preview = await uploadWithProgress('/api/backup/restore', 'POST', formData, percent => setProgress(app, percent));
      renderPreview(preview);
    } catch (error) {
      previewDiv.innerHTML = `<div class="alert alert-danger">Error: ${escapeHtml(error.message)}</div>`;
    } finally {
      document.getElementById('progress').style.display = 'none';
      checkButton.disabled = false;
    }
  });

  loadStatus();
});
