import { QueryCache, QueryClient } from "@tanstack/react-query";
import { DEFAULT_CACHE_TIME, DEFAULT_STALE_TIME } from "./constants";

export const queryClient = new QueryClient({
   queryCache: new QueryCache({}),
   logger: {
      error: (args) => {},
      log: console.log,
      warn: console.warn,
   },
   defaultOptions: {
      queries: {
         refetchOnWindowFocus: false,
         retry: false,
         cacheTime: DEFAULT_CACHE_TIME,
         staleTime: DEFAULT_STALE_TIME,
      },
   },
});
