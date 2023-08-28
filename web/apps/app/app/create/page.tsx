import React from "react";
import { Metadata } from "next";
import CreateChatGroupForm from "./CreateChatGroupForm";

export interface PageProps {}

export const metadata: Metadata = {
   title: "Create a new chat group.",
   category: "chat-group",
};

const CreateChatGroupPage = async ({}: PageProps) => {
   return (
      <section
         className={`min-h-[60vh] flex flex-col mt-8 items-center w-full`}
      >
         <h2 className={`text-default-700 text-xl`}>Create a chat group</h2>
         <CreateChatGroupForm />
      </section>
   );
};

export default CreateChatGroupPage;
