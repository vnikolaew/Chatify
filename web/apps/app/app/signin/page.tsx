"use client";
import React, { useState } from "react";
import { NextPage } from "next";
import { RegularSignInModel, useRegularSignInMutation } from "@web/api";
import Link from "next/link";
import { router } from "next/client";
import { useRouter } from "next/navigation";

const SignInPage: NextPage = () => {
   const [signInModel, setSignInModel] = useState<RegularSignInModel>({
      email: "",
      password: "",
   });
   const router = useRouter();
   const { data, mutateAsync: signIn, error } = useRegularSignInMutation();

   function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
      setSignInModel((m) => ({
         ...m,
         [e.target.name]: e.target.value,
      }));
   }

   async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
      e.preventDefault();
      console.log(signInModel);
      try {
         await signIn(signInModel);
         router.push(`/`);
      } catch (e) {
         console.error(e);
      }
   }

   return (
      <div className={`w-1/4 mx-auto mt-12`}>
         <form className={`flex flex-col gap-3`} onSubmit={handleSubmit}>
            <label className={`text-lg`} htmlFor={"email"}>
               Email:{" "}
            </label>
            <input
               className={`px-2 border-2 border-gray-200 focus:border-blue-500 py-1 text-md rounded-lg`}
               onChange={handleChange}
               value={signInModel.email}
               type={"email"}
               name={"email"}
               id={"email"}
            />
            <label className={`text-lg`} htmlFor={"password"}>
               Password:{" "}
            </label>
            <input
               className={`px-2 border-2 border-gray-200 focus:border-blue-500 py-1 text-md rounded-lg`}
               onChange={handleChange}
               value={signInModel.password}
               type={"password"}
               name={"password"}
               id={"password"}
            />
            <div className={`w-full flex items-center justify-between`}>
               <Link
                  className={`hover:underline text-md self-end text-blue-500`}
                  href={`/`}
               >
                  &larr; Go back
               </Link>
               <button
                  className={`text-white mt-4 w-1/2 py-1 text-[1.2rem] hover:opacity-80 self-end bg-blue-500 shadow-sm rounded-md`}
                  type={`submit`}
               >
                  Sign In
               </button>
            </div>
         </form>
      </div>
   );
};

export default SignInPage;
