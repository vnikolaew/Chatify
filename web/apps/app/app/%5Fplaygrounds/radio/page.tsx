"use client";
import React, { useState } from "react";
import Link from "next/link";
import { Radio, RadioGroup } from "@nextui-org/react";

const RadioGroupsPage = () => {
   const [selectedCity, setSelectedCity] = useState("London");

   return (
      <div className={`flex m-8 w-1/3 flex-col gap-8`}>
         <div className={`flex flex-col gap-3`}>
            <RadioGroup
               onValueChange={setSelectedCity}
               value={selectedCity}
               label={"Choose a city"}
            >
               <Radio description={"Capital of UK"} value={"London"}>
                  London
               </Radio>
               <Radio value={"NY"}>New York</Radio>
               <Radio value={"Paris"}>Paris</Radio>
               <Radio description={"Capital of Spain"} value={"Madrid"}>
                  Madrid
               </Radio>
               <Radio value={"Berlin"}>Berlin</Radio>
            </RadioGroup>
            <div className={`text-default-500`}>
               Selected city: {selectedCity}
            </div>
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

export default RadioGroupsPage;
