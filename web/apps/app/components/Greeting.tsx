"use client";
import React, { useMemo } from "react";
import { getImagesBaseUrl, useGetMyClaimsQuery } from "@web/api";
import {
   Button,
   getKeyValue,
   Link,
   Skeleton,
   Table,
   TableBody,
   TableCell,
   TableColumn,
   TableHeader,
   TableRow,
   User,
} from "@nextui-org/react";
import NextLink from "next/link";
import { AxiosError } from "axios";
import process from "process";

function isUserLoggedIn() {
   const cookies = document.cookie.split("; ");
   for (let cookie of cookies) {
      const [name, value] = cookie.split("=");
      if (name === process.env.NEXT_PUBLIC_APPLICATION_COOKIE_NAME) {
         console.log("User has app cookie ...");
         return true;
      }
   }

   return false;
}

export function isValidURL(url: string | null) {
   // Regular expression pattern to match a valid absolute URL
   const urlPattern = /^(https?|ftp):\/\/[^\s/$.?#].\S*$/i;

   return urlPattern.test(url);
}

export interface GreetingProps {}

const Greeting = ({}: GreetingProps) => {
   const {
      data: me,
      error,
      isLoading,
      refetch,
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

   if (error && error instanceof AxiosError)
      return (
         <div className={`flex flex-col items-center gap-3`}>
            <span className={`text-danger-500 text-medium`}>
               An error occurred. {error.message}.
            </span>
            <Button
               onPress={(_) => refetch()}
               variant={"solid"}
               size={"md"}
               color={"primary"}
            >
               Try again.
            </Button>
         </div>
      );

   if (isLoading && isFetching)
      return (
         <div className={`w-1/2 flex flex-col gap-8 mx-auto`}>
            <div className={`flex items-center gap-4`}>
               <Skeleton className={`rounded-full w-16 h-16`} />
               <Skeleton className={`w-1/2 h-8 rounded-full`} />
            </div>
            <Skeleton className={`w-24 h-6 rounded-full`} />
            <Skeleton className={`w-2/3 rounded-lg h-72`} />
         </div>
      );

   return (
      username && (
         <div>
            <div className={`flex items-center gap-4`}>
               <User
                  classNames={{
                     base: "gap-4",
                  }}
                  description={
                     <Link
                        href="https://twitter.com/jrgarciadev"
                        size="sm"
                        as={NextLink}
                        isExternal
                     >
                        @{username}
                     </Link>
                  }
                  avatarProps={{
                     src: isValidURL(me.claims.picture)
                        ? me.claims.picture
                        : `${getImagesBaseUrl()}/${me.claims.picture}`,
                     size: "lg",
                     alt: "profile-picture",
                     radius: "full",
                     color: "default",
                     isBordered: true,
                  }}
                  name={username}
               />
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
                                    ? "font-light truncate max-w-[300px]"
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
