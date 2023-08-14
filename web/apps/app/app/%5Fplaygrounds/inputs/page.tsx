"use client";
import React from "react";
import Link from "next/link";
import { Input, InputProps } from "@nextui-org/react";

const InputsPage = () => {
   const sizes: InputProps["size"][] = ["sm", "md", "lg"];

   return (
      <div className={`flex m-8 flex-col items-center gap-8`}>
         <div className={`flex-col flex gap-4`}>
            {sizes.map((s, i) => (
               <>
                  <h2>Size: {s}</h2>
                  <div
                     key={i}
                     className={`items-center grid grid-cols-3 gap-4`}
                  >
                     <Input
                        color={"default"}
                        radius={"sm"}
                        isClearable
                        size={s}
                        labelPlacement={i === 1 ? "outside" : "inside"}
                        description={i === 1 ? "Email description" : null}
                        label={"Email"}
                        type={"email"}
                     />
                     <Input
                        color={"primary"}
                        radius={"lg"}
                        isReadOnly
                        variant={"underlined"}
                        value={"jdoe@gmail.com"}
                        label={"Email"}
                        size={s}
                        type={"email"}
                     />
                     <Input
                        radius={"md"}
                        placeholder={"Enter you email"}
                        color={"success"}
                        size={s}
                        variant={"bordered"}
                        isRequired
                        label={"Email"}
                        isClearable
                        type={"email"}
                     />
                     <Input
                        color={"secondary"}
                        radius={"full"}
                        variant={"faded"}
                        size={s}
                        isClearable
                        type={"text"}
                     />
                     <Input
                        type="url"
                        label="Website"
                        placeholder="nextui.org"
                        labelPlacement="outside"
                        startContent={
                           <div className="pointer-events-none flex items-center">
                              <span className="text-default-400 text-small">
                                 https://
                              </span>
                           </div>
                        }
                     />
                     <Input
                        className={``}
                        errorMessage={"Please enter a valid search term!"}
                        color={"danger"}
                        variant={"flat"}
                        isClearable
                        size={s}
                        placeholder={"Search ..."}
                        type={"text"}
                     />
                  </div>
               </>
            ))}
         </div>
         <Link
            className={`hover:underline ml-auto self-end text-xl text-blue-500`}
            href={`/_playgrounds`}
         >
            Go Back
         </Link>
      </div>
   );
};

export default InputsPage;
