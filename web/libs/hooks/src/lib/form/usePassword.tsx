"use client";
import { useMemo, useState } from "react";

const PASSWORD_REGEX =
   /^(?=.*\d)(?=.*[A-Z])(?=.*[!@#$%^&*()_+])[A-Za-z\d!@#$%^&*()_+]{3,}$/;

export function usePassword() {
   const [password, setPassword] = useState(``);
   const validationState = useMemo(
      () =>
         password.length > 0
            ? PASSWORD_REGEX.test(password)
               ? "valid"
               : "invalid"
            : ("valid" as const),
      [password]
   );
   const errorMessage = useMemo<string | null>(() => {
      return (
         validationState === "invalid" && "Please provide a valid password."
      );
   }, [validationState]);

   return { password, setPassword, errorMessage, validationState };
}
