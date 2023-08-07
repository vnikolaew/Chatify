"use client";
import React, { useMemo, useState } from "react";

function IndexPage() {
   const [count, setCount] = useState(0);
   const doubleCount = useMemo(() => count * 2, [count]);

   return (
      <div
         className={`text-3xl flex gap-3 flex-col shadow-md shadow-gray-300 w-fit px-4 py-2 text-gray-700 rounded-md `}
      >
         <React.Profiler
            id={"1"}
            onRender={(
               id,
               phase,
               actualDuration,
               baseDuration,
               startTime,
               commitTime,
               interactions
            ) => {
               console.log(
                  id,
                  phase,
                  actualDuration,
                  baseDuration,
                  startTime,
                  commitTime,
                  interactions
               );
            }}
         >
            @Chatify
         </React.Profiler>
         <button
            className={`bg-blue-500 text-white rounded-lg shadow-md`}
            onClick={(_) => setCount((c) => c + 1)}
         >
            {count}
         </button>
         <span className={``}>Count x 2: {doubleCount}</span>
      </div>
   );
}

export default IndexPage;
