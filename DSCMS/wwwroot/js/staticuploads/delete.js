// Static Files Delete Page
import { escapeHtml, formatBytes } from './common.js';

document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const pathParts = window.location.pathname.split('/');
  const uploadId = pathParts[pathParts.length - 1];

  try {
    const response = await fetch(`/api/staticuploads/${uploadId}`);
    if (!response.ok) throw new Error('Failed to load static file');

    const upload = await response.json();

    app.innerHTML = `
      <h2>Delete Static File</h2>
      <h3>Are you sure you want to delete this?</h3>
      <div class="alert alert-warning">
        All ${upload.fileCount} file${upload.fileCount === 1 ? '' : 's'} under <code>${escapeHtml(upload.folderUrl)}</code>
        will be removed. Any links to them in layouts, templates or content will stop working.
      </div>
      <dl class="dl-horizontal">
        <dt>Folder</dt>
        <dd>${escapeHtml(upload.folderPath)}</dd>
        <dt>Uploaded File</dt>
        <dd>${escapeHtml(upload.originalFileName)}</dd>
        <dt>Size</dt>
        <dd>${formatBytes(upload.totalSizeBytes)}</dd>
        ${upload.description ? `<dt>Description</dt><dd>${escapeHtml(upload.description)}</dd>` : ''}
      </dl>
      <form id="deleteForm">
        <button type="submit" class="btn btn-danger">Delete</button>
        <a href="/Admin/StaticUploads" class="btn btn-default">Cancel</a>
      </form>
      <div id="error" class="alert alert-danger" style="display:none;"></div>
    `;

    const form = document.getElementById('deleteForm');
    const errorDiv = document.getElementById('error');

    form.addEventListener('submit', async (e) => {
      e.preventDefault();

      try {
        const deleteResponse = await fetch(`/api/staticuploads/${uploadId}`, {
          method: 'DELETE'
        });

        if (!deleteResponse.ok) {
          throw new Error('Failed to delete static file');
        }

        window.location.href = '/Admin/StaticUploads';
      } catch (error) {
        errorDiv.textContent = `Error: ${error.message}`;
        errorDiv.style.display = 'block';
      }
    });
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading static file: ${escapeHtml(error.message)}</div>`;
  }
});
