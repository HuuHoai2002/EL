import { useAuthContext } from "@asgardeo/auth-react";

const HomePage = () => {
  const { state, getAccessToken, signIn, signOut } = useAuthContext();
  console.log("🚀 ~ HomePage ~ state:", state);

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

export default HomePage;
