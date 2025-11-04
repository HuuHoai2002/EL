import { Provider } from "@/components/ui/provider.tsx";
import { RouterProvider } from "react-router/dom";

import { AuthProvider } from "@asgardeo/auth-react";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { wso2Config } from "./configs/wso2.ts";
import "./index.css";
import { router } from "./router.tsx";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <Provider>
      <AuthProvider config={wso2Config}>
        <RouterProvider router={router} />
      </AuthProvider>
    </Provider>
    ;
  </StrictMode>
);
