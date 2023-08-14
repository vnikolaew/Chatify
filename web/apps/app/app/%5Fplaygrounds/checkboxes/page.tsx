"use client";
import React, { useState } from "react";
import { Checkbox, CheckboxGroup } from "@nextui-org/react";
import HeartIcon from "./HeartIcon";
import Link from "next/link";

const CheckboxesPage = () => {
   const [isSelected, setIsSelected] = useState(false);
   const [selectedOptions, setSelectedOptions] = useState<string[]>([]);

   return (
      <div className={`flex m-8 w-max flex-col gap-8`}>
         <div className={`grid gap-4 grid-cols-4`}>
            <Checkbox
               onValueChange={setIsSelected}
               isSelected={isSelected}
               defaultSelected
            >
               Option
            </Checkbox>
            <span>
               Selected:{" "}
               <span className={`text-default-500 ml-2`}>
                  {isSelected ? "true" : "false"}
               </span>
            </span>
            <Checkbox isDisabled defaultSelected>
               Option
            </Checkbox>
            <Checkbox isDisabled>Option</Checkbox>
            <Checkbox color={"primary"} radius={"md"}>
               Option
            </Checkbox>
            <Checkbox icon={<HeartIcon />} color={"danger"} radius={"lg"}>
               Option
            </Checkbox>
            <Checkbox color={"warning"} radius={"full"}>
               Option
            </Checkbox>
            <Checkbox lineThrough color={"success"} radius={"sm"}>
               Option
            </Checkbox>
         </div>

         <div className={`flex flex-col gap-2`}>
            <CheckboxGroup
               value={selectedOptions}
               onValueChange={setSelectedOptions}
               isRequired
               color={"danger"}
               orientation={"horizontal"}
               label={`Select options: `}
               title={`Select options`}
            >
               <Checkbox color={"secondary"} value={`item-1`}>
                  Option 1
               </Checkbox>
               <Checkbox color={"secondary"} value={`item-2`}>
                  Option 2
               </Checkbox>
               <Checkbox lineThrough value={`item-3`}>
                  Option 3
               </Checkbox>
               <Checkbox value={`item-4`}>Option 4</Checkbox>
            </CheckboxGroup>
            <span className={`text-default-500`}>
               {selectedOptions.join(", ")}
            </span>
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

export default CheckboxesPage;
