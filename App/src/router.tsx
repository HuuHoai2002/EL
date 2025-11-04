import { createBrowserRouter } from "react-router";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <div>Hello World</div>,
    loader: async (e) => {
      console.log("Loader called", e);
    },
  },
]);
