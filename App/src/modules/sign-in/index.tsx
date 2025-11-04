import { useAuthContext } from "@asgardeo/auth-react";
import React from "react";

const SignInPage = () => {
  const { state, getAccessToken, signIn, signOut } = useAuthContext();
  console.log("🚀 ~ SignInPage ~ state:", state);

  React.useEffect(() => {
    if (!state.isAuthenticated) return;

    (async () => {
      const token = await getAccessToken();
      console.log("🚀 ~ SignInPage ~ access token:", token);
    })();
  }, [getAccessToken, state.isAuthenticated]);

  return (
    <div>
      <h1>Home Page</h1>
      {!state.isAuthenticated ? (
        <button onClick={() => signIn()}>Sign In</button>
      ) : (
        <button onClick={() => signOut()}>Sign Out</button>
      )}
    </div>
  );
};

export default SignInPage;
