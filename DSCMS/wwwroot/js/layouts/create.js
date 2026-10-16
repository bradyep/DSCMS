// Layouts Create Page
document.addEventListener('DOMContentLoaded', () => {
  const app = document.getElementById('app');

  function renderLayoutValueRow(key, value) {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><input type="text" class="form-control lv-key" /></td>
      <td><input type="text" class="form-control lv-value" /></td>
      <td><button type="button" class="btn btn-default btn-sm lv-remove">Remove</button></td>
    `;
    tr.querySelector('.lv-key').value = key || '';
    tr.querySelector('.lv-value').value = value || '';
    tr.querySelector('.lv-remove').addEventListener('click', () => tr.remove());
    return tr;
  }

  app.innerHTML = `
    <h2>Create Layout</h2>
    <form id="createForm" class="form-horizontal">
      <div class="form-group">
        <label for="name" class="col-md-2 control-label">Name</label>
        <div class="col-md-10">
          <input type="text" id="name" name="name" class="form-control" required />
        </div>
      </div>
      <div class="form-group">
        <label for="layoutSource" class="col-md-2 control-label">Layout Source</label>
        <div class="col-md-10">
          <input type="text" id="layoutSource" name="layoutSource" class="form-control" required
                 placeholder="/Views/DSCMS/Layouts/_YourLayout.cshtml" />
        </div>
      </div>
      <div class="form-group">
        <label for="sourceTypeId" class="col-md-2 control-label">Source Type ID</label>
        <div class="col-md-10">
          <input type="number" id="sourceTypeId" name="sourceTypeId" class="form-control" value="1" required />
          <small class="form-text text-muted">1 = RazorFile</small>
        </div>
      </div>
      <div class="form-group">
        <label class="col-md-2 control-label">Layout Values</label>
        <div class="col-md-10">
          <table class="table" id="layoutValuesTable">
            <thead><tr><th>Key</th><th>Value</th><th></th></tr></thead>
            <tbody id="layoutValuesBody"></tbody>
          </table>
          <button type="button" id="addValueRow" class="btn btn-default btn-sm">+ Add Value</button>
        </div>
      </div>
      <div class="form-group">
        <div class="col-md-offset-2 col-md-10">
          <button type="submit" class="btn btn-primary">Create</button>
          <a href="/Admin/Layouts" class="btn btn-default">Cancel</a>
        </div>
      </div>
    </form>
    <div id="error" class="alert alert-danger" style="display:none;"></div>
  `;

  const layoutValuesBody = document.getElementById('layoutValuesBody');
  document.getElementById('addValueRow').addEventListener('click', () => {
    layoutValuesBody.appendChild(renderLayoutValueRow('', ''));
  });

  const form = document.getElementById('createForm');
  const errorDiv = document.getElementById('error');

  form.addEventListener('submit', async (e) => {
    e.preventDefault();

    const layoutValues = Array.from(layoutValuesBody.querySelectorAll('tr'))
      .map(row => ({
        key: row.querySelector('.lv-key').value,
        value: row.querySelector('.lv-value').value
      }))
      .filter(v => v.key.trim() !== '');

    const data = {
      name: document.getElementById('name').value,
      layoutSource: document.getElementById('layoutSource').value,
      sourceTypeId: parseInt(document.getElementById('sourceTypeId').value),
      layoutValues: layoutValues
    };

    try {
      const response = await fetch('/api/layouts', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.title || 'Failed to create layout');
      }

      window.location.href = '/Admin/Layouts';
    } catch (error) {
      errorDiv.textContent = `Error: ${error.message}`;
      errorDiv.style.display = 'block';
    }
  });
});
