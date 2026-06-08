// Users Create Page
document.addEventListener('DOMContentLoaded', () => {
  const app = document.getElementById('app');
  app.innerHTML = `
    <h2>Create User</h2>
    <form id="createForm" class="form-horizontal">
      <div class="form-group">
        <label for="displayName" class="col-md-2 control-label">Display Name</label>
        <div class="col-md-10"><input type="text" id="displayName" class="form-control" /></div>
      </div>
      <div class="form-group">
        <label for="email" class="col-md-2 control-label">Email</label>
        <div class="col-md-10"><input type="email" id="email" class="form-control" required /></div>
      </div>
      <div class="form-group">
        <label for="userName" class="col-md-2 control-label">Username</label>
        <div class="col-md-10"><input type="text" id="userName" class="form-control" /></div>
      </div>
      <div class="form-group">
        <label for="password" class="col-md-2 control-label">Password</label>
        <div class="col-md-10"><input type="password" id="password" class="form-control" required /></div>
      </div>
      <div class="form-group">
        <div class="col-md-offset-2 col-md-10">
          <button type="submit" class="btn btn-primary">Create</button>
          <a href="/Admin/Users" class="btn btn-default">Cancel</a>
        </div>
      </div>
    </form>
    <div id="error" class="alert alert-danger" style="display:none;"></div>
  `;

  document.getElementById('createForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const data = {
      displayName: document.getElementById('displayName').value,
      email: document.getElementById('email').value,
      userName: document.getElementById('userName').value,
      password: document.getElementById('password').value
    };
    try {
      const response = await fetch('/api/users', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      });
      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.errors ? errorData.errors.join(', ') : 'Failed to create user');
      }
      window.location.href = '/Admin/Users';
    } catch (error) {
      document.getElementById('error').textContent = `Error: ${error.message}`;
      document.getElementById('error').style.display = 'block';
    }
  });
});
