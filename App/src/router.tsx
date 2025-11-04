import { createBrowserRouter } from "react-router";
import HomePage from "./modules/home";
import SignInPage from "./modules/sign-in";

export const router = createBrowserRouter([
  {
    path: "/",
    Component: HomePage,
  },
  {
    path: "/sign-in",
    Component: SignInPage,
  },
]);
