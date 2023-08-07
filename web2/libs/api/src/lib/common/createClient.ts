import axios, { AxiosRequestConfig, HttpStatusCode } from "axios";

export const createClient = (
   endpoint: string = "",
   config?: AxiosRequestConfig
) => {
   const client = axios.create({
      baseURL: `${process.env["BASE_API_URL"] ?? ""}/${endpoint}`,
      headers: {
         "Content-Type": "application/json; charset=utf-8",
         "Accept": "*/*",
      },
      validateStatus: (status) => status < HttpStatusCode.InternalServerError,
      withCredentials: true,
      cancelToken: axios.CancelToken.source().token,
      ...config,
   });

   return client;
};
