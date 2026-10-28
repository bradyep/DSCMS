// Static Files Index Page
import { escapeHtml, formatBytes, formatDate, urlField, bindCopyButtons } from './common.js';

document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');

  try {
    const response = await fetch('/api/staticuploads');
    if (!response.ok) throw new Error('Failed to load static files');

    const uploads = await response.json();

    app.innerHTML = `
      <h2>Static Files</h2>
      <p>Uploaded files are served under <code>/static/</code>. Use the URLs below to link to them from layouts, templates and content.</p>
      <p><a href="/Admin/StaticUploads/Create" class="btn btn-primary">Upload New</a></p>
      ${uploads.length === 0 ? '<p class="text-muted">Nothing has been uploaded yet.</p>' : `
      <table class="table">
        <thead>
          <tr>
            <th>Folder</th>
            <th>URL</th>
            <th>Files</th>
            <th>Size</th>
            <th>Uploaded</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          ${uploads.map(upload => `
            <tr>
              <td>
                ${escapeHtml(upload.folderPath)}
                ${upload.description ? `<br /><small class="text-muted">${escapeHtml(upload.description)}</small>` : ''}
              </td>
              <td style="min-width: 300px;">${urlField(upload.entryUrl || upload.folderUrl)}</td>
              <td>${upload.fileCount}</td>
              <td>${formatBytes(upload.totalSizeBytes)}</td>
              <td>${formatDate(upload.lastReplacedAt || upload.uploadedAt)}</td>
              <td>
                <a href="/Admin/StaticUploads/Details/${upload.staticUploadId}">Details</a> |
                <a href="/Admin/StaticUploads/Delete/${upload.staticUploadId}">Delete</a>
              </td>
            </tr>
          `).join('')}
        </tbody>
      </table>`}
    `;

    bindCopyButtons(app);
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading static files: ${escapeHtml(error.message)}</div>`;
  }
});
