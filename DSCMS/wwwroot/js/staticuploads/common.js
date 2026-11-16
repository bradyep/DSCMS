// Shared helpers for the Static Files admin pages

// File names come from uploaded archives, so never interpolate them unescaped
export function escapeHtml(value) {
  return String(value ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

export function formatBytes(bytes) {
  if (!bytes) return '0 B';
  const units = ['B', 'KB', 'MB', 'GB'];
  const i = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), units.length - 1);
  return `${(bytes / Math.pow(1024, i)).toFixed(i === 0 ? 0 : 1)} ${units[i]}`;
}

export function formatDate(value) {
  return value ? new Date(value).toLocaleString() : '';
}

// Rough client-side preview of the server's folder normalization
export function suggestFolderPath(fileName) {
  return fileName
    .replace(/\.[^.]+$/, '')
    .toLowerCase()
    .replace(/[^a-z0-9._-]+/g, '-')
    .replace(/-{2,}/g, '-')
    .replace(/^[-.]+|[-.]+$/g, '');
}

// A readonly URL box with a copy button; wire it up with bindCopyButtons()
export function urlField(url) {
  return `
    <div class="input-group input-group-sm">
      <input type="text" class="form-control" readonly value="${escapeHtml(url)}" />
      <span class="input-group-btn">
        <button type="button" class="btn btn-default copy-url">Copy</button>
      </span>
    </div>
  `;
}

export function bindCopyButtons(root) {
  root.querySelectorAll('.copy-url').forEach(button => {
    button.addEventListener('click', async () => {
      const input = button.closest('.input-group').querySelector('input');
      try {
        await navigator.clipboard.writeText(input.value);
      } catch {
        input.select();
        document.execCommand('copy');
      }
      button.textContent = 'Copied!';
      setTimeout(() => { button.textContent = 'Copy'; }, 1500);
    });
  });
}

// fetch() can't report upload progress, so uploads go through XMLHttpRequest
export function uploadWithProgress(url, method, formData, onProgress) {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open(method, url);
    xhr.upload.addEventListener('progress', e => {
      if (e.lengthComputable) onProgress(Math.round((e.loaded / e.total) * 100));
    });
    xhr.addEventListener('load', () => {
      let body = null;
      try { body = JSON.parse(xhr.responseText); } catch { /* non-JSON error page */ }
      if (xhr.status >= 200 && xhr.status < 300) {
        resolve(body);
      } else {
        reject(new Error(errorMessage(xhr.status, body)));
      }
    });
    xhr.addEventListener('error', () => reject(new Error('Upload failed. The file may be larger than the server allows.')));
    xhr.send(formData);
  });
}

function errorMessage(status, body) {
  if (body?.error) return body.error;
  if (body?.errors) return Object.values(body.errors).flat().join(' ');
  if (body?.title) return body.title;
  if (status === 413) return 'The file is larger than the server allows.';
  return `Upload failed (HTTP ${status}).`;
}

export function progressBar() {
  return `
    <div id="progress" class="progress" style="display:none;">
      <div class="progress-bar" role="progressbar" style="width: 0%;">0%</div>
    </div>
  `;
}

export function setProgress(root, percent) {
  const container = root.querySelector('#progress');
  const bar = container.querySelector('.progress-bar');
  container.style.display = 'block';
  bar.style.width = `${percent}%`;
  bar.textContent = percent < 100 ? `${percent}%` : 'Processing...';
}
