// Static Files Create (Upload) Page
import { suggestFolderPath, uploadWithProgress, progressBar, setProgress } from './common.js';

document.addEventListener('DOMContentLoaded', () => {
  const app = document.getElementById('app');

  app.innerHTML = `
    <h2>Upload Static File</h2>
    <form id="createForm" class="form-horizontal">
      <div class="form-group">
        <label for="file" class="col-md-2 control-label">File</label>
        <div class="col-md-10">
          <input type="file" id="file" name="file" class="form-control" required />
          <small class="form-text text-muted">A single file (PDF, HTML, image, ...) or a .zip containing a whole folder, such as an HTML5 game.</small>
        </div>
      </div>
      <div class="form-group" id="extractGroup" style="display:none;">
        <div class="col-md-offset-2 col-md-10">
          <div class="checkbox">
            <label><input type="checkbox" id="extract" checked /> Extract the .zip (uncheck to serve the .zip file itself)</label>
          </div>
        </div>
      </div>
      <div class="form-group">
        <label for="folderPath" class="col-md-2 control-label">Folder</label>
        <div class="col-md-10">
          <input type="text" id="folderPath" name="folderPath" class="form-control" placeholder="e.g. resume or games/shapeshifter" />
          <small class="form-text text-muted">Files will be served from <code id="urlPreview">/static/&hellip;/</code>. Defaults to the file name.</small>
        </div>
      </div>
      <div class="form-group">
        <label for="description" class="col-md-2 control-label">Description</label>
        <div class="col-md-10">
          <input type="text" id="description" name="description" class="form-control" maxlength="500" />
        </div>
      </div>
      <div class="form-group">
        <div class="col-md-offset-2 col-md-10">
          ${progressBar()}
          <button type="submit" id="submit" class="btn btn-primary">Upload</button>
          <a href="/Admin/StaticUploads" class="btn btn-default">Cancel</a>
        </div>
      </div>
    </form>
    <div id="error" class="alert alert-danger" style="display:none;"></div>
  `;

  const form = document.getElementById('createForm');
  const fileInput = document.getElementById('file');
  const extractGroup = document.getElementById('extractGroup');
  const extractInput = document.getElementById('extract');
  const folderInput = document.getElementById('folderPath');
  const urlPreview = document.getElementById('urlPreview');
  const submitButton = document.getElementById('submit');
  const errorDiv = document.getElementById('error');

  // Keep suggesting a folder from the file name until the user types their own
  let folderEdited = false;

  function updatePreview() {
    const folder = folderInput.value.trim() || folderInput.placeholder;
    urlPreview.textContent = `/static/${folder.replace(/^\/+|\/+$/g, '')}/`;
  }

  fileInput.addEventListener('change', () => {
    const file = fileInput.files[0];
    extractGroup.style.display = file && file.name.toLowerCase().endsWith('.zip') ? 'block' : 'none';
    if (file && !folderEdited) {
      folderInput.value = suggestFolderPath(file.name);
    }
    updatePreview();
  });

  folderInput.addEventListener('input', () => {
    folderEdited = folderInput.value.trim() !== '';
    updatePreview();
  });

  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    errorDiv.style.display = 'none';

    const formData = new FormData();
    formData.append('file', fileInput.files[0]);
    formData.append('folderPath', folderInput.value.trim());
    formData.append('description', document.getElementById('description').value.trim());
    formData.append('extract', extractInput.checked ? 'true' : 'false');

    submitButton.disabled = true;
    try {
      const created = await uploadWithProgress('/api/staticuploads', 'POST', formData, percent => setProgress(app, percent));
      window.location.href = `/Admin/StaticUploads/Details/${created.staticUploadId}`;
    } catch (error) {
      document.getElementById('progress').style.display = 'none';
      errorDiv.textContent = `Error: ${error.message}`;
      errorDiv.style.display = 'block';
      submitButton.disabled = false;
    }
  });
});
