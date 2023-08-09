"use client";
import React, { useMemo } from "react";
import { useGetMyClaimsQuery } from "@web/api";

export const APPLICATION_COOKIE_NAME = ".AspNetCore.Identity.Application";

function isUserLoggedIn() {
   const cookies = document.cookie.split("; ");
   for (let cookie of cookies) {
      const [name, value] = cookie.split("=");
      if (name === APPLICATION_COOKIE_NAME) {
         console.log("User has app cookie ...");
         return true;
      }
   }

   return false;
}

const Greeting = () => {
   const {
      data: me,
      error,
      isLoading,
   } = useGetMyClaimsQuery({
      enabled: isUserLoggedIn(),
   });

   const username = useMemo<string | null>(() => {
      if (!me || !me?.claims || !Object.keys(me.claims).length) return null;

      return Object.entries(me?.claims ?? []).find(([key, value]) =>
         key.endsWith("name")
      )[1] as string;
   }, [me]);
   console.log(me && me.claims);
   return (
      username && (
         <div>
            Greetings, <b className={`text-xl`}>{username}</b> !
         </div>
      )
   );
};

export default Greeting;
