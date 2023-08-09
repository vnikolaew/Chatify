"use client";
import React, { useState } from "react";
import { NextPage } from "next";
import { RegularSignUpModel, useRegularSignUpMutation } from "@web/api";
import Link from "next/link";
import { useRouter } from "next/navigation";

const SignUpPage: NextPage = () => {
   const router = useRouter();

   const [signUpModel, setSignUpModel] = useState<RegularSignUpModel>({
      email: "",
      password: "",
      username: "",
   });
   const {
      data,
      mutateAsync: signUp,
      error,
      isLoading,
   } = useRegularSignUpMutation();

   function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
      setSignUpModel((m) => ({
         ...m,
         [e.target.name]: e.target.value,
      }));
   }

   async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
      e.preventDefault();
      try {
         await signUp(signUpModel);
         router.push(`/`);
      } catch (e) {
         console.error(e);
      }
   }

   return (
      <div className={`w-1/4 mx-auto mt-12`}>
         <form
            autoComplete={"off"}
            className={`flex flex-col gap-3`}
            onSubmit={handleSubmit}
         >
            <label className={`text-lg`} htmlFor={"username"}>
               Username:{" "}
            </label>
            <input
               className={`px-3 border-2 border-gray-200 focus:border-blue-500 py-2 text-md rounded-lg`}
               autoComplete={"off"}
               onChange={handleChange}
               value={signUpModel.username}
               type={"text"}
               name={"username"}
               id={"username"}
            />
            <label className={`text-lg`} htmlFor={"email"}>
               Email:{" "}
            </label>
            <input
               className={`px-3 border-2 border-gray-200 focus:border-blue-500 py-2 text-md rounded-lg`}
               onChange={handleChange}
               value={signUpModel.email}
               type={"email"}
               name={"email"}
               id={"email"}
            />
            <label className={`text-lg`} htmlFor={"password"}>
               Password:{" "}
            </label>
            <input
               className={`px-3 border-2 border-gray-200 focus:border-blue-500 py-2 text-md rounded-lg`}
               onChange={handleChange}
               value={signUpModel.password}
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
                  disabled={isLoading}
                  className={`text-white disabled:opacity-30 mt-4 w-1/2 py-1 text-[1.2rem] hover:opacity-80 self-end bg-blue-500 shadow-sm rounded-md`}
                  type={`submit`}
               >
                  {isLoading ? "Loading ..." : "Sign Up"}
               </button>
            </div>
         </form>
      </div>
   );
};

export default SignUpPage;
