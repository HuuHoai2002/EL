import { PUBLIC_ROUTE_PATHS } from "@/shared/constants";
import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import Cookies from "js-cookie";

let isRefreshing = false;

let failedRequests: Array<{
  resolve: (value?: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedRequests.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedRequests = [];
};

const forceLogout = () => {
  Cookies.remove("accessToken");
  Cookies.remove("refreshToken");

  const curPath = window.location.pathname.slice(1);
  if (!PUBLIC_ROUTE_PATHS.includes(curPath)) {
    window.location.href = "/dang-nhap";
  }
};

const createAxiosClient = () => {
  const baseUrl = window.location.origin + "/api";
  const instance = axios.create({
    baseURL: baseUrl,
    headers: { "Content-Type": "application/json" },
  });

  instance.interceptors.request.use(
    (config) => {
      const accessToken = Cookies.get("accessToken");
      if (accessToken) {
        config.headers["Authorization"] = `Bearer ${accessToken}`;
      }
      return config;
    },
    (error) => Promise.reject(error)
  );

  instance.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
      const originalRequest = error.config as InternalAxiosRequestConfig & {
        _retry?: boolean;
      };

      if (!originalRequest) {
        return Promise.reject(error);
      }

      const refreshToken = Cookies.get("refreshToken");

      if (error.response?.status === 401 && !originalRequest._retry) {
        if (!refreshToken) {
          forceLogout();
          return Promise.reject(error);
        }

        if (isRefreshing) {
          return new Promise((resolve, reject) => {
            failedRequests.push({ resolve, reject });
          })
            .then((token) => {
              originalRequest.headers["Authorization"] = `Bearer ${token}`;
              return instance.request(originalRequest);
            })
            .catch((err) => Promise.reject(err));
        }

        originalRequest._retry = true;
        isRefreshing = true;

        try {
          // const { data: response } = await api.getNewAccessToken(refreshToken);

          // if (response?.success && response.data?.accessToken) {
          //   const accessToken = response.data.accessToken;
          //   Cookies.set("accessToken", accessToken);

          //   originalRequest.headers["Authorization"] = `Bearer ${accessToken}`;

          //   processQueue(null, accessToken);
          //   return instance.request(originalRequest);
          // } else {
          //   processQueue(error, null);
          //   forceLogout();
          //   return Promise.reject(error);
          // }
          return instance.request(originalRequest);
        } catch (refreshError: any) {
          if (refreshError.response?.status === 400) {
            processQueue(refreshError, null);
            forceLogout();
            return Promise.reject(refreshError);
          }

          processQueue(refreshError, null);
          return Promise.reject(refreshError);
        } finally {
          isRefreshing = false;
        }
      }

      return Promise.reject(error);
    }
  );

  return instance;
};

export const axiosClient = createAxiosClient();
