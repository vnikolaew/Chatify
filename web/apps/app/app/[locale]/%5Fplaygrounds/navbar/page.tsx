"use client";
import React, { useState } from "react";
import {
   Navbar,
   NavbarBrand,
   NavbarContent,
   NavbarItem,
   NavbarMenu,
   Link,
   NavbarMenuItem,
   NavbarMenuToggle,
   Avatar,
   Input,
} from "@nextui-org/react";

const NavbarPage = () => {
   const [isMenuOpen, setIsMenuOpen] = useState(false);

   return (
      <div className={`flex min-h-[150vh] m-8 flex-col items-center gap-8`}>
         <div className={`flex-col w-full flex gap-4`}>
            <Navbar
               classNames={{
                  item: "data-[active=true]:text-primary-500",
               }}
               onMenuOpenChange={setIsMenuOpen}
               isMenuOpen={isMenuOpen}
               isBordered
               shouldHideOnScroll
               className={`w-full`}
            >
               <NavbarBrand>Brand</NavbarBrand>
               <NavbarContent>
                  {[1, 2, 3, 4].map((i) => (
                     <NavbarItem isActive={i === 2} key={i}>
                        <Link
                           color={i === 2 ? "primary" : "foreground"}
                           href={`#`}
                        >
                           Item {i}
                        </Link>
                     </NavbarItem>
                  ))}
               </NavbarContent>
               <NavbarMenu>
                  {[1, 2, 3, 4, 5].map((i) => (
                     <NavbarMenuItem key={i}>
                        <Link
                           className={`w-full`}
                           size={"lg"}
                           color={"primary"}
                           href={`#`}
                        >
                           Item {i}
                        </Link>
                     </NavbarMenuItem>
                  ))}
                  <NavbarMenuItem>
                     <Link
                        onPress={(_) => setIsMenuOpen(false)}
                        color={"danger"}
                        href={`#`}
                     >
                        Close
                     </Link>
                  </NavbarMenuItem>
               </NavbarMenu>
               <NavbarContent className={`flex items-center justify-end`}>
                  <Input
                     placeholder={"Type to search ..."}
                     size={"md"}
                     type={"search"}
                  />
                  <Avatar
                     getInitials={(c) => c.charAt(0)}
                     size={"md"}
                     className={`py-0`}
                     name={"John Doe"}
                     color={"secondary"}
                     isBordered
                     // as={"button"}
                  />
               </NavbarContent>
            </Navbar>
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

export default NavbarPage;
