// ContentTypes Edit - similar structure to create, loads existing data first
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const pathParts = window.location.pathname.split('/');
  const id = pathParts[pathParts.length - 1];

  try {
    const [ctResponse, optionsResponse] = await Promise.all([
      fetch(`/api/contenttypes/${id}`),
      fetch('/api/contenttypes/form-options')
    ]);

    const ct = await ctResponse.json();
   const options = await optionsResponse.json();

    app.innerHTML = `<h2>Edit Content Type</h2><form id="editForm"><!-- similar fields as create --></form>`;

    // Form submission with PUT to /api/contenttypes/${id}
    document.getElementById('editForm').addEventListener('submit', async (e) => {
      e.preventDefault();
      // PUT logic here
      window.location.href = '/Admin/ContentTypes';
    });
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
  }
});
