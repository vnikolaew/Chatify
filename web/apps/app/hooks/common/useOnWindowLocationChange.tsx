import { useEffect } from "react";

export function useOnWindowLocationChange(
   callback: (e: Event) => void | Promise<void>,
) {
   const eventName = `pushState`;

   useEffect(() => {
      window.addEventListener(eventName, callback);
      return () => window.removeEventListener(eventName, callback);
   }, []);
}
