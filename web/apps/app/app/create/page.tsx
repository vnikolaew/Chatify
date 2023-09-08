import React from "react";
import { Metadata } from "next";
import CreateChatGroupForm from "./CreateChatGroupForm";
import CreateChatGroupHeading from "./CreateChatGroupHeading";

export interface PageProps {}

export const metadata: Metadata = {
   title: "Create a new chat group.",
   category: "chat-group",
};

const CreateChatGroupPage = async ({}: PageProps) => {
   return (
      <section
         className={`min-h-[60vh] flex flex-col mt-12 items-center w-full`}
      >
         <div className={` flex flex-col items-start`}>
            <CreateChatGroupHeading className={`mb-2`} />
            <CreateChatGroupForm />
         </div>
      </section>
   );
};

export default CreateChatGroupPage;
