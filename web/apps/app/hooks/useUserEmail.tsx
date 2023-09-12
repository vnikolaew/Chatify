"use client";
import { useMemo, useState } from "react";

const EMAIL_REGEX = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;

export function useUserEmail() {
   const [userEmail, setUserEmail] = useState(``);
   const validationState = useMemo(
      () =>
         userEmail.length > 0
            ? EMAIL_REGEX.test(userEmail)
               ? "valid"
               : "invalid"
            : ("valid" as const),
      [userEmail]
   );
   const errorMessage = useMemo<string | null>(() => {
      return (
         validationState === "invalid" &&
         "Please provide a valid user email (e.g. user123@test.com)."
      );
   }, [validationState]);

   return { userEmail, setUserEmail, errorMessage, validationState };
}
