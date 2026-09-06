export const environment = {
  production: false,
  environment: 'development',

  // Empty in both environments now. `ng serve` proxies /api to localhost:5033 (see
  // proxy.conf.json) exactly as nginx proxies it to the backend container in production,
  // so development is same-origin too. That matters for more than tidiness: the session
  // is a cookie, and a cross-origin dev setup needs CORS credentials and a SameSite
  // relaxation that production does not.
  apiHost: '',
};
