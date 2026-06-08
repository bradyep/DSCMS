// Details page - view content details
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

    // Set edit link
    document.getElementById('editLink').href = `/Admin/Contents/Edit/${content.contentId}`;

  } catch (error) {
    console.error('Error loading content:', error);
    alert('Error loading content details. Please try again.');
  }
})();
