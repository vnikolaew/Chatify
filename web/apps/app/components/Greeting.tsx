"use client";
import React, { useMemo } from "react";
import { useGetMyClaimsQuery } from "@web/api";
import Image from "next/image";

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

export interface GreetingProps {
   imagesBaseUrl: string;
}

const Greeting = ({ imagesBaseUrl }: GreetingProps) => {
   const {
      data: me,
      error,
      isLoading,
      isFetching,
   } = useGetMyClaimsQuery({
      enabled: isUserLoggedIn(),
   });

   const username = useMemo<string | null>(() => {
      if (!me || !me?.claims || !Object.keys(me.claims).length) return null;

      return Object.entries(me?.claims ?? []).find(([key, value]) =>
         key.endsWith("name")
      )[1] as string;
   }, [me]);

   if (isLoading && isFetching) return <h3>Loading ...</h3>;

   return (
      username && (
         <div>
            <div>
               Greetings, <b className={`text-xl`}>{username}</b> !
            </div>
            <Image
               className={`rounded-md shadow-sm`}
               width={80}
               height={80}
               crossOrigin={"use-credentials"}
               decoding={"async"}
               src={`${imagesBaseUrl}/${me.claims.picture}`}
               alt={"profile-picture"}
            />
            <table className={`text-sm border my-4 border-blue-500 rounded-md`}>
               <thead>
                  <tr className={`border border-blue-500`}>
                     <th className={`border-blue-500 border`}>Name</th>
                     <th>Value</th>
                  </tr>
               </thead>
               <tbody>
                  {Object.entries(me?.claims)
                     .filter(([_, value]) => !!value)
                     .map(([key, value]) => (
                        <tr className={`border border-blue-500`}>
                           <td
                              className={`border-blue-500 font-bold py-1 px-2 border`}
                           >
                              {key}
                           </td>
                           <td className={`border-blue-500 py-1 px-2 border`}>
                              {value as string}
                           </td>
                        </tr>
                     ))}
               </tbody>
            </table>
         </div>
      )
   );
};

export default Greeting;
