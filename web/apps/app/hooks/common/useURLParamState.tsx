import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

export function useURLParamState(paramName: string, defaultValue: string) {
   const [value, setValue] = useState(
      () =>
         new URL(window?.location?.href).searchParams?.get(paramName) ??
         defaultValue
   );
   const router = useRouter();

   useEffect(() => {
      const url = new URL(window?.location?.href);
      url.searchParams.set(paramName, value);
      router.replace(url.href);
   }, [value]);

   return [value, setValue] as const;
}
