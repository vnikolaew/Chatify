import { useMemo, useState } from "react";

const USER_HANDLE_REGEX = /^[\w-]+#\d{4}$/;

export function useUserHandle() {
   const [userHandle, setUserHandle] = useState("");
   const validationState = useMemo(
      () =>
         userHandle.length > 0
            ? USER_HANDLE_REGEX.test(userHandle)
               ? "valid"
               : "invalid"
            : ("valid" as const),
      [userHandle]
   );
   const errorMessage = useMemo<string | null>(() => {
      return (
         validationState === "invalid" &&
         "Please type a valid user handle (e.g. Jack#1234)."
      );
   }, [validationState]);

   return { userHandle, setUserHandle, errorMessage, validationState };
}
