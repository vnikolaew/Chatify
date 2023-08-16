import React, { Fragment } from "react";
import Link from "next/link";
import Greeting from "../components/Greeting";
import { cookies } from "next/headers";
import SignOut from "../components/SignOut";
import * as process from "process";

const APPLICATION_COOKIE_NAME = ".AspNetCore.Identity.Application";

export const revalidate = 0;

async function IndexPage() {
   const cookieStore = cookies();
   const isUserLoggedIn = !!cookieStore.get(APPLICATION_COOKIE_NAME);

   return (
      <div
         className={`flex min-h-[60vh] text-xl gap-3 flex-col items-center shadow-gray-300 w-full px-4 py-4 rounded-md `}
      >
         <h1 className={`text-3xl my-4`}>Home Page</h1>
         {isUserLoggedIn && (
            <Greeting
               imagesBaseUrl={`${process.env.NEXT_PUBLIC_BACKEND_API_URL.slice(
                  0,
                  -4
               )}/static`}
            />
         )}
         <div className={`mt-12`}>
            <Link
               className={`hover:underline text-blue-500`}
               href={`_playgrounds`}
            >
               Go to playgrounds
            </Link>
         </div>
         {!isUserLoggedIn ? (
            <Fragment>
               <h2 className={`font-bold`}>You are currently not logged in.</h2>
               <div className={`flex items-center gap-8`}>
                  <Link
                     className={`hover:underline text-blue-600`}
                     href={`/signup`}
                  >
                     {" "}
                     Sign Up
                  </Link>
                  <Link
                     className={`hover:underline text-blue-600`}
                     href={`/signin`}
                  >
                     Sign In
                  </Link>
               </div>
            </Fragment>
         ) : (
            <div className={`self-center mt-2`}>
               <SignOut />
            </div>
         )}
      </div>
   );
}

export default IndexPage;
