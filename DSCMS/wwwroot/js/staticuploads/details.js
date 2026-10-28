// Static Files Details Page
import {
  escapeHtml, formatBytes, formatDate, urlField, bindCopyButtons,
  uploadWithProgress, progressBar, setProgress
} from './common.js';

document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const pathParts = window.location.pathname.split('/');
  const uploadId = pathParts[pathParts.length - 1];

  async function load() {
    const response = await fetch(`/api/staticuploads/${uploadId}`);
    if (!response.ok) throw new Error('Failed to load static file');
    return response.json();
  }

  function render(upload) {
    app.innerHTML = `
      <h2>Static File Details</h2>
      <dl class="dl-horizontal">
        <dt>Folder</dt>
        <dd>${escapeHtml(upload.folderPath)}</dd>
        ${upload.entryUrl ? `<dt>Main URL</dt><dd style="max-width: 600px;">${urlField(upload.entryUrl)}</dd>` : ''}
        <dt>Folder URL</dt>
        <dd style="max-width: 600px;">${urlField(upload.folderUrl)}</dd>
        <dt>Uploaded File</dt>
        <dd>${escapeHtml(upload.originalFileName)}${upload.isArchive ? ' (extracted)' : ''}</dd>
        <dt>Size</dt>
        <dd>${formatBytes(upload.totalSizeBytes)} in ${upload.fileCount} file${upload.fileCount === 1 ? '' : 's'}</dd>
        <dt>Uploaded</dt>
        <dd>${formatDate(upload.uploadedAt)}${upload.uploadedByUserName ? ` by ${escapeHtml(upload.uploadedByUserName)}` : ''}</dd>
        ${upload.lastReplacedAt ? `<dt>Last Replaced</dt><dd>${formatDate(upload.lastReplacedAt)}</dd>` : ''}
      </dl>

      <form id="descriptionForm" class="form-inline" style="margin-bottom: 20px;">
        <div class="form-group">
          <label for="description">Description</label>
          <input type="text" id="description" class="form-control" maxlength="500" style="min-width: 400px;" />
        </div>
        <button type="submit" class="btn btn-default">Save</button>
        <span id="descriptionSaved" class="text-success" style="display:none;">Saved</span>
      </form>

      <h3>Files</h3>
      <table class="table table-condensed">
        <thead><tr><th>Path</th><th>Type</th><th>Size</th><th>SHA-256</th></tr></thead>
        <tbody>
          ${upload.files.map(file => `
            <tr>
              <td><a href="${escapeHtml(file.url)}" target="_blank" rel="noopener noreferrer">${escapeHtml(file.relativePath)}</a></td>
              <td>${escapeHtml(file.contentType)}</td>
              <td>${formatBytes(file.sizeBytes)}</td>
              <td><code title="${escapeHtml(file.sha256)}">${escapeHtml(file.sha256.substring(0, 12))}&hellip;</code></td>
            </tr>
          `).join('')}
        </tbody>
      </table>

      <h3>Replace Files</h3>
      <p class="text-muted">Upload a new file or .zip to replace everything in this folder. The folder and its URLs stay the same.</p>
      <form id="replaceForm" class="form-horizontal">
        <div class="form-group">
          <div class="col-md-6">
            <input type="file" id="file" class="form-control" required />
          </div>
          <div class="col-md-6">
            <div class="checkbox" id="extractGroup" style="display:none;">
              <label><input type="checkbox" id="extract" checked /> Extract the .zip</label>
            </div>
          </div>
        </div>
        ${progressBar()}
        <button type="submit" id="replaceSubmit" class="btn btn-warning">Replace</button>
      </form>
      <div id="error" class="alert alert-danger" style="display:none; margin-top: 15px;"></div>

      <div style="margin-top: 20px;">
        <a href="/Admin/StaticUploads/Delete/${upload.staticUploadId}" class="btn btn-danger">Delete</a>
        <a href="/Admin/StaticUploads" class="btn btn-default">Back to List</a>
      </div>
    `;

    bindCopyButtons(app);

    const errorDiv = document.getElementById('error');
    const showError = message => {
      errorDiv.textContent = `Error: ${message}`;
      errorDiv.style.display = 'block';
    };

    const descriptionInput = document.getElementById('description');
    descriptionInput.value = upload.description || '';
    document.getElementById('descriptionForm').addEventListener('submit', async (e) => {
      e.preventDefault();
      errorDiv.style.display = 'none';
      try {
        const response = await fetch(`/api/staticuploads/${uploadId}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ description: descriptionInput.value.trim() })
        });
        if (!response.ok) throw new Error('Failed to save description');
        const saved = document.getElementById('descriptionSaved');
        saved.style.display = 'inline';
        setTimeout(() => { saved.style.display = 'none'; }, 1500);
      } catch (error) {
        showError(error.message);
      }
    });

    const fileInput = document.getElementById('file');
    const extractGroup = document.getElementById('extractGroup');
    fileInput.addEventListener('change', () => {
      const file = fileInput.files[0];
      extractGroup.style.display = file && file.name.toLowerCase().endsWith('.zip') ? 'block' : 'none';
    });

    document.getElementById('replaceForm').addEventListener('submit', async (e) => {
      e.preventDefault();
      errorDiv.style.display = 'none';

      const formData = new FormData();
      formData.append('file', fileInput.files[0]);
      formData.append('extract', document.getElementById('extract').checked ? 'true' : 'false');

      const submitButton = document.getElementById('replaceSubmit');
      submitButton.disabled = true;
      try {
        const updated = await uploadWithProgress(`/api/staticuploads/${uploadId}/file`, 'PUT', formData, percent => setProgress(app, percent));
        render(updated);
      } catch (error) {
        document.getElementById('progress').style.display = 'none';
        showError(error.message);
        submitButton.disabled = false;
      }
    });
  }

  try {
    render(await load());
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading static file: ${escapeHtml(error.message)}</div>`;
  }
});
