import React from "react";

export interface PageProps {

}

const Page = ({}: PageProps) => {
    return (
        <section className={`min-h-[70vh] m-8`}>
            <h2>About Page</h2>
        </section>
    );
};

export default Page;
