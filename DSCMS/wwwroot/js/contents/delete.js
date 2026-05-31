// Delete page - delete confirmation
(async function() {
  try {
    const contentId = document.getElementById('contentIdHolder').dataset.contentid;

    const response = await fetch(`/api/contents/${contentId}`);

    if (!response.ok) {
      alert('Content not found');
      window.location.href = '/Admin/Contents';
      return;
    }

    const content = await response.json();

    // Populate details
    document.getElementById('bodySource').textContent = content.bodySource || '';
    document.getElementById('creationDate').textContent = new Date(content.creationDate).toLocaleDateString();
    document.getElementById('lastUpdatedDate').textContent = new Date(content.lastUpdatedDate).toLocaleDateString();
    document.getElementById('title').textContent = content.title || '';
    document.getElementById('urlToDisplay').textContent = content.urlToDisplay || '';

  } catch (error) {
    console.error('Error loading content:', error);
    alert('Error loading content details. Please try again.');
  }
})();

document.getElementById('deleteForm').addEventListener('submit', async function(e) {
  e.preventDefault();

  const contentId = document.getElementById('contentIdHolder').dataset.contentid;

  try {
    const response = await fetch(`/api/contents/${contentId}`, {
      method: 'DELETE'
    });

    if (response.ok || response.status === 204) {
      window.location.href = '/Admin/Contents';
    } else {
      alert('Error deleting content. Please try again.');
    }
  } catch (error) {
    console.error('Error deleting content:', error);
    alert('Error deleting content. Please try again.');
  }
});
