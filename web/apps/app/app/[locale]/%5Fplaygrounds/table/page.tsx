"use client";
import {
   Table,
   TableHeader,
   TableRow,
   TableCell,
   TableColumn,
   TableBody,
   Spinner,
   getKeyValue,
} from "@nextui-org/react";
import React, { useEffect, useState } from "react";
import Link from "next/link";
import axios from "axios";
import { sleep } from "@web/api";

export interface IUser {
   id: number;
   name: string;
   username: string;
   email: string;
   address: Address;
   phone: string;
   website: string;
   company: Company;
}

export interface Address {
   street: string;
   suite: string;
   city: string;
   zipcode: string;
   geo: Geo;
}

export interface Geo {
   lat: string;
   lng: string;
}

export interface Company {
   name: string;
   catchPhrase: string;
   bs: string;
}

const TablePage = () => {
   const [users, setUsers] = useState<IUser[]>(null!);
   const [selectedUserKeys, setSelectedUserKeys] = useState(new Set(["2"]));

   useEffect(() => {
      axios
         .get(`https://jsonplaceholder.typicode.com/users`)
         .then(async (res) => {
            console.log(res.data);
            await sleep(1000);
            setUsers(res.data);
         });
   }, []);

   if (!users) return <Spinner size={"lg"} color={"danger"} />;

   return (
      <div className={`flex m-8 w-1/2 flex-col gap-8`}>
         <div className={`flex flex-col items-center gap-4`}>
            <Table
               onRowAction={(key) => console.log(key)}
               selectionMode={"multiple"}
               selectedKeys={selectedUserKeys}
               onSelectionChange={setSelectedUserKeys as any}
               color={"primary"}
               aria-label="Example static collection table"
            >
               <TableHeader
                  columns={
                     [
                        {
                           key: "id",
                           label: "Id",
                        },
                        {
                           key: "company",
                           label: "Company",
                        },
                        {
                           key: "username",
                           label: "Username",
                        },
                        {
                           key: "email",
                           label: "Email",
                        },
                        {
                           key: "phone",
                           label: "Phone",
                        },
                        {
                           key: "website",
                           label: "Website",
                        },
                     ] as { key: keyof IUser; label: string }[]
                  }
               >
                  {(column) => (
                     <TableColumn className={`text-large`} key={column.key}>
                        {column.label}
                     </TableColumn>
                  )}
               </TableHeader>
               <TableBody
                  emptyContent={"No rows to show."}
                  items={users.map((u, i) => {
                     const {
                        id,
                        username,
                        phone,
                        website,
                        company: { name },
                        email,
                     } = u;
                     return {
                        id,
                        username,
                        phone,
                        website,
                        company: name,
                        email,
                        key: (i + 1).toString(),
                     };
                  })}
               >
                  {(item) => (
                     <TableRow key={item.key}>
                        {(columnKey) => (
                           <TableCell className={`text-medium`}>
                              {getKeyValue(item, columnKey)}
                           </TableCell>
                        )}
                     </TableRow>
                  )}
               </TableBody>
            </Table>
            <p className={`mt-4`}>
               Selected users:{" "}
               {((selectedUserKeys as any) === "all"
                  ? []
                  : [...selectedUserKeys]
               )
                  .map((i) => users[Number(i) - 1].username)
                  .join(", ")}
            </p>
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

export default TablePage;
