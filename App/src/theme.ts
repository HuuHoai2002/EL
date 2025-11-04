import { createSystem, defaultConfig, defineConfig } from "@chakra-ui/react";

const config = defineConfig({
  theme: {
    tokens: {
      colors: {
        primary: { value: "#0FEE0F" },
      },
      fonts: {
        body: { value: "Roboto, sans-serif" },
      },
    },
  },
});

export const system = createSystem(defaultConfig, config);
