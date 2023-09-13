import useCookie from "react-use-cookie";
import process from "process";

export function useIsUserLoggedIn() {
   const [isUserLoggedIn] = useCookie(
      process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME
   );
   return { isUserLoggedIn: !!isUserLoggedIn };
}
