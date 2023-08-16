"use client";
import React, { useMemo } from "react";
import { useGetMyClaimsQuery } from "@web/api";
import Image from "next/image";
import {
   Avatar,
   CircularProgress,
   getKeyValue,
   Spinner,
   Table,
   TableBody,
   TableCell,
   TableColumn,
   TableHeader,
   TableRow,
} from "@nextui-org/react";

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

function isValidURL(url: string | null) {
   // Regular expression pattern to match a valid absolute URL
   const urlPattern = /^(https?|ftp):\/\/[^\s/$.?#].[^\s]*$/i;

   return urlPattern.test(url);
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
      useErrorBoundary: (error, query) => false,
   });

   const username = useMemo<string | null>(() => {
      if (!me || !me?.claims || !Object.keys(me.claims).length) return null;

      return Object.entries(me?.claims ?? []).find(([key, value]) =>
         key.endsWith("name")
      )[1] as string;
   }, [me]);

   if (isLoading && isFetching)
      return (
         <span>
            <Spinner
               classNames={{
                  label: "text-small text-default-500",
               }}
               size={"md"}
               color={"danger"}
               labelColor={"foreground"}
               label={"Loading ..."}
            />
         </span>
      );

   return (
      username && (
         <div>
            <div className={`flex items-center gap-4`}>
               <Avatar
                  src={
                     isValidURL(me.claims.picture)
                        ? me.claims.picture
                        : `${imagesBaseUrl}/${me.claims.picture}`
                  }
                  className={``}
                  size={"lg"}
                  alt={"profile-picture"}
                  // name={me.claims.}
                  radius={"full"}
                  color={"default"}
                  isBordered
               />
               <h2>
                  Greetings, <b className={`text-xl`}>{username}</b> !
               </h2>
            </div>
            <h2 className={`mt-8`}>Claims: </h2>
            <Table className={`mt-4`}>
               <TableHeader
                  columns={[
                     { key: "type", label: "Type" },
                     { key: "value", label: "Value" },
                  ]}
               >
                  {(column) => (
                     <TableColumn
                        isRowHeader
                        className={`text-medium text-center`}
                        key={column.key}
                     >
                        {column.label}
                     </TableColumn>
                  )}
               </TableHeader>
               <TableBody
                  items={Object.entries(me.claims).map(([name, value]) => ({
                     type: name,
                     value,
                  }))}
               >
                  {(item: Record<string, any>) => (
                     <TableRow key={item.type}>
                        {(columnKey) => (
                           <TableCell
                              className={
                                 columnKey === "value"
                                    ? "font-light"
                                    : "font-medium"
                              }
                           >
                              {getKeyValue(item, columnKey)}
                           </TableCell>
                        )}
                     </TableRow>
                  )}
               </TableBody>
            </Table>
         </div>
      )
   );
};

export default Greeting;
