// Layouts Edit Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const pathParts = window.location.pathname.split('/');
  const layoutId = pathParts[pathParts.length - 1];

  function renderLayoutValueRow(layoutValueId, key, value) {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>
        <input type="hidden" class="lv-id" value="${layoutValueId || 0}" />
        <input type="text" class="form-control lv-key" />
      </td>
      <td><input type="text" class="form-control lv-value" /></td>
      <td><button type="button" class="btn btn-default btn-sm lv-remove">Remove</button></td>
    `;
    tr.querySelector('.lv-key').value = key || '';
    tr.querySelector('.lv-value').value = value || '';
    tr.querySelector('.lv-remove').addEventListener('click', () => tr.remove());
    return tr;
  }

  try {
    const response = await fetch(`/api/layouts/${layoutId}`);
    if (!response.ok) throw new Error('Failed to load layout');

    const layout = await response.json();

    app.innerHTML = `
      <h2>Edit Layout</h2>
      <form id="editForm" class="form-horizontal">
        <input type="hidden" id="layoutId" value="${layout.layoutId}" />
        <div class="form-group">
          <label for="name" class="col-md-2 control-label">Name</label>
          <div class="col-md-10">
            <input type="text" id="name" name="name" class="form-control" value="${layout.name}" required />
          </div>
        </div>
        <div class="form-group">
          <label for="layoutSource" class="col-md-2 control-label">Layout Source</label>
          <div class="col-md-10">
            <input type="text" id="layoutSource" name="layoutSource" class="form-control" value="${layout.layoutSource}" required />
          </div>
        </div>
        <div class="form-group">
          <label for="sourceTypeId" class="col-md-2 control-label">Source Type ID</label>
          <div class="col-md-10">
            <input type="number" id="sourceTypeId" name="sourceTypeId" class="form-control" value="${layout.sourceTypeId}" required />
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
            <button type="submit" class="btn btn-primary">Save</button>
            <a href="/Admin/Layouts" class="btn btn-default">Cancel</a>
          </div>
        </div>
      </form>
      <div id="error" class="alert alert-danger" style="display:none;"></div>
    `;

    const layoutValuesBody = document.getElementById('layoutValuesBody');
    (layout.layoutValues || []).forEach(lv => {
      layoutValuesBody.appendChild(renderLayoutValueRow(lv.layoutValueId, lv.key, lv.value));
    });

    document.getElementById('addValueRow').addEventListener('click', () => {
      layoutValuesBody.appendChild(renderLayoutValueRow(0, '', ''));
    });

    const form = document.getElementById('editForm');
    const errorDiv = document.getElementById('error');

    form.addEventListener('submit', async (e) => {
      e.preventDefault();

      const layoutValues = Array.from(layoutValuesBody.querySelectorAll('tr'))
        .map(row => ({
          layoutValueId: parseInt(row.querySelector('.lv-id').value) || 0,
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
        const updateResponse = await fetch(`/api/layouts/${layoutId}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(data)
        });

        if (!updateResponse.ok) {
          const errorData = await updateResponse.json();
          throw new Error(errorData.title || 'Failed to update layout');
        }

        window.location.href = '/Admin/Layouts';
      } catch (error) {
        errorDiv.textContent = `Error: ${error.message}`;
        errorDiv.style.display = 'block';
      }
    });
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading layout: ${error.message}</div>`;
  }
});
