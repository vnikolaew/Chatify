import { useEffect } from "react";

export function useOnWindowLocationChange(
   callback: (e: Event) => void | Promise<void>
) {
   useEffect(() => {
      window.addEventListener("pushState", callback);
      return () => window.removeEventListener(`pushState`, callback);
   }, []);
}
