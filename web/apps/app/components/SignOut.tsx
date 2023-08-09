"use client";
import React from "react";
import { useSignOutMutation } from "@web/api";
import { useRouter } from "next/navigation";
import { useQueryClient } from "@tanstack/react-query";

const SignOut = () => {
   const router = useRouter();
   const client = useQueryClient();
   const {
      mutateAsync: signOut,
      data,
      isLoading,
      error,
   } = useSignOutMutation();

   const handleClick = async () => {
      try {
         await signOut(null!);
         // client.clear();
         await client.resetQueries(
            ["me", "claims"],
            { exact: false },
            { cancelRefetch: true }
         );
      } catch (e) {}
   };

   return (
      <div>
         <button
            onClick={(_) => signOut(null!)}
            disabled={isLoading}
            className={`hover:underline text-blue-500`}
         >
            {isLoading ? "Loading ..." : "Sign Out"}
         </button>
      </div>
   );
};

export default SignOut;
