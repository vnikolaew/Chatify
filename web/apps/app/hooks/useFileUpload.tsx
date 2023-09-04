import React, { useMemo, useRef, useState } from "react";
import { ChatifyFile } from "@components/chat-group/messages/editor/MessageTextEditor";
import { v4 as uuidv4 } from "uuid";

export function useFileUpload() {
   const [attachedFiles, setAttachedFiles] = useState<ChatifyFile[]>([]);

   // Mapping of File ID -> File Object URL
   const attachedFilesUrls = useMemo<Map<string, string>>(
      () =>
         new Map<string, string>(
            attachedFiles.map((file) => [
               file.id,
               URL.createObjectURL(file.file),
            ])
         ),
      [attachedFiles]
   );
   const fileUploadRef = useRef<HTMLInputElement>(null!);

   const handleRemoveFile = (id: string) =>
      setAttachedFiles((files) => files.filter((_) => _.id !== id));

   const clearFiles = () => setAttachedFiles([]);

   const handleFileUpload: React.ChangeEventHandler<HTMLInputElement> = async ({
      target: { files },
   }) => {
      const newFiles = Array.from({ length: files.length }).map((_, i) =>
         files.item(i)
      );
      setAttachedFiles((files) => [
         ...files,
         ...newFiles.map((_) => new ChatifyFile(_, uuidv4())),
      ]);
   };

   return {
      attachedFiles,
      attachedFilesUrls,
      handleFileUpload,
      fileUploadRef,
      clearFiles,
      handleRemoveFile,
   };
}