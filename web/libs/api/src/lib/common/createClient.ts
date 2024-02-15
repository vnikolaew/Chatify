import axios, { AxiosRequestConfig, HttpStatusCode } from "axios";
import { isServer, sleep } from "../utils";

export const USER_LOCATION_LOCAL_STORAGE_KEY = "user-geolocation";

export const createClient = (
   endpoint: string = "",
   config?: AxiosRequestConfig
) => {
   const client = axios.create({
      baseURL: `/api/${endpoint}`,
      headers: {
         "Content-Type": "application/json; charset=utf-8",
         "Accept": "*/*",
         "X-Api-Call": `true`,
         "X-User-Locale": isServer() ? "" : window.navigator.language,
         "X-User-Location": ``,
      },
      validateStatus: (status) => status < HttpStatusCode.InternalServerError,
      withCredentials: true,
      cancelToken: axios.CancelToken.source().token,
      ...config,
   });

   client.interceptors.request.use((config) => {
      if (isServer()) return config;

      const userLocation = localStorage.getItem(
         USER_LOCATION_LOCAL_STORAGE_KEY
      ) as string;
      if (!userLocation) return config;

      const [latitude, longitude] = userLocation?.split(";");
      config.headers.set("X-User-Location", `${latitude};${longitude}`);
      return config;
   });

   client.interceptors.response.use(async (value) => {
      await sleep(1000);

      if (value.status === HttpStatusCode.Unauthorized) {
         const currentPath = window.location.pathname;
         const returnUrl = encodeURIComponent(currentPath);

         window.location.replace(`/signin?returnUrl=${returnUrl}`);
         return value;
      }
      return value;
   });

   return client;
};
