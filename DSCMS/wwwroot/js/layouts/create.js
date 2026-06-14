// Layouts Create Page
document.addEventListener('DOMContentLoaded', () => {
  const app = document.getElementById('app');

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
        <div class="col-md-offset-2 col-md-10">
          <button type="submit" class="btn btn-primary">Create</button>
          <a href="/Admin/Layouts" class="btn btn-default">Cancel</a>
        </div>
      </div>
    </form>
    <div id="error" class="alert alert-danger" style="display:none;"></div>
  `;

  const form = document.getElementById('createForm');
  const errorDiv = document.getElementById('error');

  form.addEventListener('submit', async (e) => {
    e.preventDefault();

    const data = {
      name: document.getElementById('name').value,
      layoutSource: document.getElementById('layoutSource').value,
      sourceTypeId: parseInt(document.getElementById('sourceTypeId').value)
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
