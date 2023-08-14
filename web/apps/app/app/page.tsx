import React, { Fragment } from "react";
import Link from "next/link";
import Greeting from "../components/Greeting";
import { cookies } from "next/headers";
import SignOut from "../components/SignOut";
import * as process from "process";

const APPLICATION_COOKIE_NAME = ".AspNetCore.Identity.Application";

async function IndexPage() {
   const cookieStore = cookies();
   const isUserLoggedIn = !!cookieStore.get(APPLICATION_COOKIE_NAME);

   return (
      <div
         className={`flex text-xl gap-3 flex-col shadow-gray-300 w-fit px-4 py-2 rounded-md `}
      >
         <h1 className={`text-3xl`}>Home Page</h1>
         <Greeting
            imagesBaseUrl={`${process.env.NEXT_PUBLIC_BACKEND_API_URL.slice(
               0,
               -4
            )}/static`}
         />
         <div className={`my-4`}>
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
                     className={`hover:underline text-blue-500`}
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
            <div className={`self-end mt-2`}>
               <SignOut />
            </div>
         )}
      </div>
   );
}

export default IndexPage;
