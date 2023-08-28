"use client";
import React, { useMemo, useRef, useState } from "react";
import { Formik } from "formik";
import { CreateChatGroupModel, useCreateChatGroupMutation } from "@web/api";
import { Button, Chip, Image, Input, Spinner } from "@nextui-org/react";
import UploadIcon from "@components/icons/UploadIcon";
import TooltipButton from "@components/TooltipButton";
import { useRouter } from "next/navigation";

export interface CreateChatGroupFormProps {}

const CreateChatGroupForm = ({}: CreateChatGroupFormProps) => {
   const fileInputRef = useRef<HTMLInputElement>();
   const router = useRouter();
   const [selectedFile, setSelectedFile] = useState<File | null>();
   const fileUrl = useMemo(() => {
      if (!selectedFile) return "";
      return URL.createObjectURL(selectedFile);
   }, [selectedFile]);
   const normalizedFileName = useMemo(() => {
      if (!selectedFile) return "";
      const parts = selectedFile?.name.split(".");
      return `${parts
         .slice(0, parts.length - 1)
         .join("")
         .substring(0, 20)}.${parts.at(-1)}`;
   }, [selectedFile]);
   const {
      isLoading,
      error,
      mutateAsync: createChatGroup,
   } = useCreateChatGroupMutation();

   const handleSubmit = async (model: CreateChatGroupModel) => {
      await createChatGroup(model, {
         onSuccess: (data) => {
            console.log(data);
            router.push(`/?c=${data.data.groupId}&new=true`);
         },
      });
      console.log(model);
      console.log("form submitted");
   };

   return (
      <Formik<CreateChatGroupModel>
         initialValues={{ about: "", file: null!, name: "" }}
         validateOnMount={false}
         validateOnChange={true}
         onSubmit={handleSubmit}
      >
         {({
            values,
            errors,
            touched,
            setFieldValue,
            handleChange,
            handleSubmit,
            isSubmitting,
         }) => (
            <form
               onSubmit={handleSubmit}
               className={`w-[320px] flex flex-col gap-3 mt-6`}
               autoComplete={"off"}
            >
               <Input
                  autoFocus
                  isClearable
                  onClear={() => setFieldValue("name", "")}
                  value={values.name}
                  onChange={handleChange}
                  labelPlacement={"inside"}
                  isRequired
                  label={"Name"}
                  classNames={{
                     input: "text-small pb-1",
                     label: "py-1",
                  }}
                  autoComplete={"off"}
                  errorMessage={errors?.name}
                  validationState={
                     touched.name && errors.name ? "invalid" : "valid"
                  }
                  placeholder={"Think of a cool name."}
                  size={"md"}
                  className={`py-1 text-md rounded-lg`}
                  type={"text"}
                  name={"name"}
                  id={"name"}
               />
               <Input
                  autoFocus
                  isClearable
                  onClear={() => setFieldValue("about", "")}
                  value={values.about}
                  onChange={handleChange}
                  labelPlacement={"inside"}
                  label={"About"}
                  classNames={{
                     input: "text-small pb-1",
                     label: "py-1",
                  }}
                  autoComplete={"off"}
                  errorMessage={errors?.about}
                  validationState={
                     touched.about && errors.about ? "invalid" : "valid"
                  }
                  placeholder={"The best community."}
                  size={"md"}
                  className={`py-1 text-md rounded-lg`}
                  type={"text"}
                  name={"about"}
                  id={"about"}
               />
               <div className={`flex items-center mt-2 gap-4`}>
                  <label className={`text-medium`}>
                     Select a group picture:
                  </label>
                  <TooltipButton
                     onClick={(_) => fileInputRef.current.click()}
                     icon={
                        <UploadIcon
                           fill={`white`}
                           className={`fill-white`}
                           size={24}
                        />
                     }
                     content={"Upload a file"}
                  />
                  <input
                     onChange={async (e) => {
                        console.log(e.target.files);
                        setSelectedFile(e.target.files[0]!);
                        await setFieldValue("file", e.target.files[0]!);
                     }}
                     // value={selectedFile}
                     ref={fileInputRef}
                     hidden
                     type={"file"}
                  />
               </div>
               {selectedFile && (
                  <div
                     className={`w-full flex gap-4 flex-col items-center justify-center`}
                  >
                     <Image
                        className={`mx-auto`}
                        radius={"md"}
                        src={fileUrl}
                        width={100}
                        height={100}
                     />
                     <Chip
                        color={"warning"}
                        variant={"faded"}
                        size={"sm"}
                        className={`text-xs py-0 px-4`}
                        content={selectedFile?.name}
                     >
                        {normalizedFileName}
                     </Chip>
                  </div>
               )}
               <Button
                  variant={"solid"}
                  isLoading={isSubmitting}
                  spinner={
                     <Spinner
                        className={`mr-2`}
                        color={"default"}
                        size={"sm"}
                     />
                  }
                  color={"primary"}
                  size={"sm"}
                  className={`text-white w-2/3 mt-8 py-4 text-small hover:opacity-80 self-center shadow-sm rounded-md`}
                  type={`submit`}
               >
                  {isSubmitting ? "Loading" : "Create"}
               </Button>
            </form>
         )}
      </Formik>
   );
};

export default CreateChatGroupForm;
