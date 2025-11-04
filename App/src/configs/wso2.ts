export const wso2Config = {
  signInRedirectURL: import.meta.env.VITE_SIGN_IN_REDIRECT_URL,
  signOutRedirectURL: import.meta.env.VITE_SIGN_OUT_REDIRECT_URL,
  clientID: import.meta.env.VITE_WSO2_CLIENT_ID,
  baseUrl: import.meta.env.VITE_WSO2_BASE_URL,
  scope: ["openid", "profile"],
};
