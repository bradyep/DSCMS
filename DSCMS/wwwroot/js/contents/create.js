// Create page - create new content
(async function() {
  try {
    // Load form options
    const response = await fetch('/api/contents/form-options');
    const options = await response.json();

    // Populate dropdowns
    populateSelect('ContentTypeId', options.contentTypes);
    populateSelect('TemplateId', options.templates);
    populateSelect('BodySourceTypeId', options.sourceTypes);

    // Store default template lookup
    window.defaultTemplateLookup = options.defaultTemplateLookup;

    // Setup ContentType change handler
    document.getElementById('ContentTypeId').addEventListener('change', onContentTypeChanged);

  } catch (error) {
    console.error('Error loading form options:', error);
    alert('Error loading form data. Please refresh the page.');
  }
})();

function populateSelect(elementId, items) {
  const select = document.getElementById(elementId);
  select.innerHTML = '';
  items.forEach(item => {
    const option = document.createElement('option');
    option.value = item.id;
    option.textContent = item.name;
    select.appendChild(option);
  });
}

function onContentTypeChanged() {
  const contentTypeId = parseInt(document.getElementById('ContentTypeId').value);
  if (window.defaultTemplateLookup && window.defaultTemplateLookup[contentTypeId]) {
    document.getElementById('TemplateId').value = window.defaultTemplateLookup[contentTypeId];
  }
}

document.getElementById('createForm').addEventListener('submit', async function(e) {
  e.preventDefault();

  const formData = {
    bodySource: document.getElementById('BodySource').value,
    bodySourceTypeId: parseInt(document.getElementById('BodySourceTypeId').value),
    contentTypeId: parseInt(document.getElementById('ContentTypeId').value),
    templateId: parseInt(document.getElementById('TemplateId').value),
    title: document.getElementById('Title').value,
    urlToDisplay: document.getElementById('UrlToDisplay').value
  };

  try {
    const response = await fetch('/api/contents', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(formData)
    });

    if (response.ok) {
      window.location.href = '/Admin/Contents';
    } else {
      const error = await response.json();
      alert('Error creating content: ' + JSON.stringify(error));
    }
  } catch (error) {
    console.error('Error creating content:', error);
    alert('Error creating content. Please try again.');
  }
});
