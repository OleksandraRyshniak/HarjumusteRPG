namespace HarjumusteRPG.API.Infrastructure;

/// <summary>Customizes the Swagger Authorize window: log in with email and password.</summary>
public static class SwaggerLoginPage
{
    public const string HeadContent = """
<style>
  .swagger-ui .dialog-ux .modal-ux-header h3 { font-size: 0; }
  .swagger-ui .dialog-ux .modal-ux-header h3::before { content: 'Sign in'; font-size: 24px; }
  .swagger-ui .dialog-ux .modal-ux-content h4 { font-size: 0; }
  .swagger-ui .dialog-ux .modal-ux-content h4::before { content: 'Pawly account'; font-size: 20px; }
  .swagger-ui .authorization__btn { display: none !important; }
</style>
<script>
(function () {
  let token = null;
  let busy = false;
  let statusText = 'Guest';

  function installInterceptor() {
    const ui = window.ui;
    if (!ui) return;
    ui.getConfigs().requestInterceptor = function (req) {
      req.headers = req.headers || {};
      if (token) {
        req.headers.Authorization = 'Bearer ' + token;
      } else {
        delete req.headers.Authorization;
        delete req.headers.authorization;
      }
      return req;
    };
  }

  // reload the document, so Swagger shows only what this user may call
  function refreshSwagger() {
    const ui = window.ui;
    if (!ui) return;
    installInterceptor();
    ui.specActions.updateUrl('/swagger/v1/swagger.json');
    ui.specActions.download('/swagger/v1/swagger.json');
  }

  function showStatus() {
    const wrapper = document.querySelector('.auth-wrapper');
    if (!wrapper) return;
    let span = document.getElementById('loginStatus');
    if (!span) {
      span = document.createElement('span');
      span.id = 'loginStatus';
      span.style.cssText = 'margin-right:12px;font-weight:bold;';
      wrapper.insertBefore(span, wrapper.firstChild);
    }
    span.textContent = statusText;
  }

  // the Authorize window says "Username", but we log in with the email
  function renameLabels() {
    const modal = document.querySelector('.modal-ux-content');
    if (!modal) return;
    const walker = document.createTreeWalker(modal, NodeFilter.SHOW_TEXT);
    let node;
    while ((node = walker.nextNode())) {
      if (/^\s*username:?\s*$/i.test(node.nodeValue)) node.nodeValue = 'Email:';
    }
  }

  async function checkAuth() {
    const ui = window.ui;
    if (!ui || !ui.authSelectors || busy) return;

    const authorized = ui.authSelectors.authorized();
    const basic = authorized && authorized.get('basicAuth');

    // the user entered email and password in the Authorize window
    if (basic && !token) {
      busy = true;
      try {
        const creds = basic.toJS().value || {};
        const response = await fetch('/api/auth/login', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ email: creds.username, password: creds.password })
        });

        if (response.ok) {
          const data = await response.json();
          token = data.token;
          statusText = 'Logged in as ' + data.user.username + ' (' + data.user.role + ')';
          refreshSwagger();
        } else {
          ui.authActions.logout(['basicAuth']);
          statusText = 'Guest';
          alert('Invalid email or password');
        }
      } catch (error) {
        ui.authActions.logout(['basicAuth']);
        statusText = 'Guest';
      }
      busy = false;
    }

    // the user pressed Logout
    if (!basic && token) {
      token = null;
      statusText = 'Guest';
      refreshSwagger();
    }

    renameLabels();
    showStatus();
  }

  setInterval(checkAuth, 300);
})();
</script>
""";
}