import axios, { AxiosRequestConfig, HttpStatusCode } from "axios";
import { __IS_SERVER__, sleep } from "../utils";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { Platform } from "react-native";

export const USER_LOCATION_LOCAL_STORAGE_KEY = "user-geolocation";

export const apiBaseUrl =
   // @ts-ignore
   process.env.NEXT_PUBLIC_BACKEND_API_URL ??
   // @ts-ignore
   process.env.EXPO_PUBLIC_BACKEND_API_URL;

export const IS_MOBILE =
   // @ts-ignore
   process.env.EXPO_PUBLIC_BACKEND_PLATFORM === `mobile`;

export const IS_WEB = !IS_MOBILE;

export const createClient = (
   endpoint: string = "",
   config?: AxiosRequestConfig
) => {
   const client = axios.create({
      baseURL: `${apiBaseUrl}/api/${endpoint}`,
      headers: {
         "Content-Type": "application/json; charset=utf-8",
         "Accept": "*/*",
         "X-Api-Call": `true`,
         "X-User-Locale": __IS_SERVER__ ? "" : window.navigator.language,
         "X-User-Location": ``,
      },
      validateStatus: (status) => status < HttpStatusCode.InternalServerError,
      withCredentials: true,
      cancelToken: axios.CancelToken.source().token,
      ...config,
   });

   if (IS_MOBILE) {
      client.interceptors.request.use(async (config) => {
         config.headers["User-Agent"] = Platform.OS;

         const cookieValue = (await AsyncStorage.getItem(`auth_cookie`)) ?? ``;
         config.headers["Cookie"] = `${
            // @ts-ignore
            process.env.EXPO_PUBLIC_APPLICATION_COOKIE_NAME as string
         }=${cookieValue}`;

         return config;
      });
      return client;
   }

   client.interceptors.request.use((config) => {
      if (__IS_SERVER__) return config;

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
