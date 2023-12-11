"use client";
import React, { useState } from "react";
import Link from "next/link";
import {
   Avatar,
   Button,
   Chip,
   Dropdown,
   DropdownItem,
   DropdownItemProps,
   DropdownMenu,
   DropdownTrigger,
} from "@nextui-org/react";

const DropdownPage = () => {
   const [selectedKeys, setSelectedKeys] = useState<Set<string | number>>(
      new Set<string>()
   );
   const [selectedValues, setSelectedValues] = useState<Set<string | number>>(
      new Set<string>()
   );

   const variants: DropdownItemProps["variant"][] = [
      "faded",
      "light",
      "solid",
      "flat",
      "shadow",
      "bordered",
   ];

   return (
      <div className={`flex m-8 flex-col items-center gap-8`}>
         <div className={`flex items-center flex-col gap-4`}>
            {variants.map((v, i) => (
               <Dropdown key={i} title={"Dropdown"}>
                  <DropdownTrigger className={``}>
                     <Button className={`w-fit px-8`} variant={v}>
                        Click
                     </Button>
                  </DropdownTrigger>
                  <DropdownMenu
                     selectionMode={"single"}
                     variant={v}
                     // onAction={alert}
                     disabledKeys={["copy"]}
                     aria-label={`Actions`}
                     {...(i === 2
                        ? {
                             selectedKeys,
                             onSelectionChange: (s) =>
                                setSelectedKeys(new Set([...s])),
                          }
                        : null)}
                  >
                     <DropdownItem key={"create"}>Create new</DropdownItem>
                     <DropdownItem key={"copy"}>Copy</DropdownItem>
                     <DropdownItem
                        key={"edit"}
                        className={`text-warning`}
                        color={"warning"}
                     >
                        Edit{" "}
                     </DropdownItem>
                     <DropdownItem
                        key={"delete"}
                        color={"danger"}
                        className={`text-danger`}
                     >
                        Delete
                     </DropdownItem>
                  </DropdownMenu>
               </Dropdown>
            ))}
            <Dropdown showArrow backdrop={"blur"}>
               <DropdownTrigger>
                  <Button variant={"bordered"}>
                     Item {[...selectedValues].join(", ")}
                  </Button>
               </DropdownTrigger>
               <DropdownMenu
                  aria-label={"Selection"}
                  selectedKeys={selectedValues}
                  onSelectionChange={(s) => setSelectedValues(new Set([...s]))}
                  disallowEmptySelection
                  selectionMode={"multiple"}
               >
                  {[1, 2, 3, 5].map((i) => (
                     <DropdownItem
                        description={`Item ${i} description.`}
                        startContent={
                           <Chip
                              className={`text-small`}
                              radius={"sm"}
                              size={`sm`}
                           >
                              chip
                           </Chip>
                        }
                        shortcut={`#${i}`}
                        key={i.toString()}
                     >
                        Item {i}
                     </DropdownItem>
                  ))}
               </DropdownMenu>
            </Dropdown>
            <span className={`mt-4`}>{[...selectedKeys].join(", ")}</span>
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

export default DropdownPage;
